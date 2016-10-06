# iMessage Bridge

**Current version: 1.0-a1 (This is only an alpha version so changes in new versions will usually be bug fixes or fixed mistakes in the source code)**

An iMessage server that allows many clients including Windows to communicate using iMessage and SMS. This project is only in the alpha stage so not all features will work properly.

The iMessage bridge comes bundled with:
- The server that is used to send out incoming messages to the clients.
- An API DLL to be used with .NET applications although other developers can also port the API to be used with Java and many other platforms.
- A simple Windows client to test the bridge. Third-party developers can design new clients for the bridge.

To download and run the server, find a Mac with iMessage set up, install the [Mono framework](http://www.mono-project.com/download/), [download the project](https://github.com/3dflash/iMessageBridge/archive/master.zip), unzip it, navigate to the iMessageBridge folder, and then simply run "iMessage Bridge.app".

Please note that the source code was developed with Visual Studio so they might not open correctly with Xamarin.

# The Server
The server is an important part of the bridge. It has two parts:
- An HTTP server
- A WebSocket stream server

Because Apple doesn't provide an API for iMessage, the server is used to detect changes from the chat.db database file and be able to send incoming messages to clients. The client can also send outgoing messages (text only) to the server and then tells AppleScript to send the message.

## HTTP Server
The HTTP server (port 9080) is used to provide information like recipients, conversations, messages and attachments to the requested client. Most REST requests follow a JSON response like this:
```json
{
    "status": "ok"
    ...
}
```
- `"status":` indicates the state of the response.
    - `"ok"` if the request was completed successfully.
    - `"error"` if an error occurs with the request.
    - `"exception"` if an internal error occurs within the server.
    - `"access denied"` if the user isn't authenticated.

The server also supports authentication to prevent access to the bridge. The username is "user" and password is "password" by default but it can be set by the settings window.

Please note that even you have authentication enabled, web traffic from the server to the client are unencrypted. Same goes for the stream server. Support for HTTPS may be added in the future.

### Supported Requests

Clients can send a request with any of the following resources:
- `GET /recipients` returns a list of recipients on the bridge.

    ```json
    {
        "status": "ok",
        "data": [
            {
                "id": 1,
                "address": "+18005555555",
                "country": "US",
                "serviceType": "iMessage",
                "name": "Johnny Appleseed",
                "hasPicture": true
            }
        ]
    }
    ```
    - `"data":` the list of recipients returned.
    - `"id":` the id of the recipient.
    - `"address":` the phone number or email of the recipient.
    - `"country":` the country of where the recipient's phone is.
    - `"serviceType":` the type of service the recipient uses.
        - `"iMessage"` the recipient uses iMessage.
        - `"SMS"` the recipient uses SMS.
    - `"name":` the name of the recipient. If a name is not found in your contacts or is not added, the address is returned.
    - `"hasPicture":` Returns true if this recipient has a contact picture set.
    
- `GET /recipientpicture?id=<recipient id>` returns the recipient's picture as a JPEG.
- `GET /conversations` returns a list of conversations on the bridge. `recipients` and `messages` are returned as ids to conserve size.

    ```json
    {
        "status": "ok",
        "data": [
            {
                "id": 1,
                "name": "Johnny Appleseed",
                "recipients": [
                    1
                ],
                "messages": [
                    100,
                    101,
                    102,
                    103,
                    104
                ],
                "serviceType": "iMessage",
                "hasGroupName": false,
                "displayName": "Johnny Appleseed"
            }
        ]
    }
    ```
    - `"data":` the list of conversations returned.
    - `"id":` the id of the conversation.
    - `"name":` the "name" of the conversation. Usually the name is shown as the person's phone number or email, or if the conversation is a group, the name may look something like this: chat539749676434700307. We recommend you use the display name when you show the conversation's name on your app.
    - `"recipients":` the list of recipients in the conversation.
    - `"messages":` the list of messages in the conversation.
    - `"serviceType":` the type of service the conversation uses.
        - `"iMessage"` the conversation uses iMessage.
        - `"SMS"` the conversation uses SMS.
    - `"hasGroupName":` Returns true if the conversation has a group name.
    - `"displayName":` Gets the display name of the conversation. If a group name was set, the group name is returned, if not, the recipients' names are returned.
    
- `GET /messages` returns a list of messages on the bridge. `from` and `attachments` are returned as ids.

    ```json
    {
        "status": "ok",
        "data": [
            {
                "id": 100,
                "from": 1,
                "fromMe": false,
                "serviceType": "iMessage",
                "date": "10/1/2016 9:31:56 AM",
                "dateRead": "10/1/2016 12:52:09 PM",
                "dateDelivered": "10/1/2016 9:31:59 AM",
                "hasRead": true,
                "hasDelivered": true,
                "hasSent": false,
                "subject": "",
                "text": "Hey dude! What's up?",
                "attachments": []
            }
        ]
    }
    ```
    - `"data":` the list of messages returned.
    - `"id":` the id of the message.
    - `"from":` who the message is from or sent to.
    - `"fromMe":` the list of recipients in the conversation.
    - `"serviceType":` the type of service the message used.
        - `"iMessage"` the message uses iMessage.
        - `"SMS"` the message uses SMS.
    - `"date":` when the message was sent.
    - `"dateRead":` when the message was read by you or the recipient. If the recipient has read receipts off, the value will be null.
    - `"dateDelivered":` when the message was delivered. If the message was sent using SMS, the value will be null.
    - `"hasRead":` returns true if the recipient or you read the message. If the recipient has read receipts off, the value will be false.
    - `"hasDelivered":` returns true if the message was delivered. If the message was sent using SMS, the value will be false.
    - `"hasSent":` returns true if the message was sent successfully. If the message was sent by you, the value will be false.
    - `"subject":` the subject of the message. Usually shown above the actual message.
    - `"text":` basically the message itself.
    - `"attachments":` the list of attachments in this message.
    
- `GET /attachments` returns a list of attachments on the bridge. An attachment is usually an image or a video that is sent along with the message.
    ```json
    {
        "status": "ok",
        "data": [
            {
                "id": 1,
                "fullFileName": "/Users/dylan/Library/Messages/Attachments/54/04/A08C881D-A7E6-4A3C-9488-F9F0427F002F/IMG_2364.jpeg",
                "fileName": "IMG_2364.jpeg",
                "createdDate": "4/13/2016 9:14:42 AM",
                "mimeType": "image/jpeg",
                "totalBytes": "751646"
            }
        ]
    }
    ```
    - `"data":` the list of attachments returned.
    - `"id":` the id of the attachment.
    - `"fullFileName":` the full file name of the attachment.
    - `"fileName":` the simplified file name of the attachment.
    - `"createdDate":` When the attachment was created.
    - `"mimeType":` the attachment's mime type.
    - `"totalBytes":` the total bytes of the attachment.
    
- `GET /attachment?id=<attachment id>` returns an attachment from the bridge. The content type is specified by the attachment's mime type.
- `POST /send?recipient=<recipient's address>&text=<text>` sends a message (text only) to a recipient. The client can specify `&sms=1` if the user want to send the message as an SMS message. 
- `GET /test` returns a test response. Mostly used to check if the server has authentication enabled.
- `GET /serverinfo` returns infomation about the bridge.
    ```json
    {
        "status": "ok",
        "bridgeVersion": "1.0-a1"
    }
    ```
    - `"bridgeVersion":` the version of the bridge or server.

## Stream server
The WebSocket stream server (port 9081) is what will notify clients about what changes are made to the chat.db database. Supposedly what objects including recipients, conversations, messages and attachments are added, updated, or removed from the database.

When the client connects to the stream, the client needs to know if the server has authentication enabled. So the server send a response like this:
```json
{
    "event": "auth",
    "needsAuth": true
}
```
- `"needAuth":` returns true if the server needs authentication. If it doesn't need it, the client starts listening right away.

To log in, simply send a JSON message like this:
```json
{
    "username": "user",
    "password": "password"
}
```
- `"username":` the username is `"user"` for now.
- `"password":` the password for the username.

When the client is logged in, it will start listening for responses, if not, the server closes the connection. A stream response looks like this:
```json
{
    "event": "update"
    ...
}
```
- `"event":` what stream event occured.
    - `"update"` the database was updated.
    - `"auth"` if the server needs authentication.

For example, when someone sends you a message, the server then sends a response like this to the client:
```json
{
	"event": "update",
	"objectType": "Message",
	"eventType": "Add",
	"obj": {
        "id": 100,
        "from": 1,
        "fromMe": false,
        "serviceType": "iMessage",
        "date": "10/1/2016 9:31:56 AM",
        "dateRead": null,
        "dateDelivered": null,
        "hasRead": false,
        "hasDelivered": true,
        "hasSent": false,
        "subject": "",
        "text": "Hey dude! What's up?",
        "attachments": []
    }
}
```
- `"objectType":` what type of object was updated.
    - `"Recipient"` a recipient object.
    - `"Conversation"` a conversation object.
    - `"Message"` a message object.
    - `"Attachment"` an attachment object.
- `"eventType":` what type of event was invoked in the database.
    - `"Add"` an object was added to the bridge database.
    - `"Update"` an object's properties was updated.
    - `"Remove"` an object was removed from the bridge database.
- `"obj":` the updated object. The object's information is referenced at the HTTP Server section.