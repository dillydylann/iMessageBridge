# iMessage Bridge .NET API

**Current version: 1.0-a1 (This is only an alpha version so changes in new versions will usually be bug fixes or fixed mistakes in the source code)**

A UWP interface for the iMessage Bridge API.

## Usage/Examples

Getting started:
```csharp
// Create the context.
BridgeContext context = new BridgeContext(/* Your bridge's IP goes here */);
if (await context.AuthenticationIsRequiredAsync()) // Check if we have to log in.
{
    bool successLogin = false;
    while (!successLogin) // Loop if the log in fails.
    {
        string username = "username goes here";
        string password = "password goes here";
        context.AuthenticationCredentials = new Credentials(username, password);
        successLogin = await context.ValidateCredentialsAsync(); // Validate username and password.
        if (successLogin)
            // Login was successful.
        else
            // Login has failed.
    }
}
await context.InitAsync(); // Initialize the context.
// We're good to go now!
```

List all the conversations:
```csharp
foreach (var c in context.Conversations)
{
    Debug.WriteLine("-- " + c.Value.DisplayName + " --");
    foreach (var m in c.Value.Messages)
        if (m.FromMe) // Is the message from you?
            Debug.WriteLine("Me: " + m.Text);
        else if (m.From != null) // I don't know why but sometimes blank messages will show with no recipient.
            Debug.WriteLine(m.From.Name + ": " + m.Text);
    Debug.WriteLine();
}
```

Listen for incoming text messages:
```csharp
context.StreamUpdate += (s, e) =>
{
    if (e.Error == null) // We have no errors.
    {
        if (e.EventType == EventType.Add && e.ObjectType == ObjectType.Message) // We got a new text message!
        {
            Message m = e.Object as Message;
            if (!m.FromMe)
                Debug.WriteLine("New text from " + m.From.Name + ": " + m.Text);
        }
    }
	else if (ev.Error.Message.ToLower().Contains("closed")) // Reconnect when the stream closes unexpectedly.
		await context.StartStreamAsync();
    else
        throw e.Error;
};
await context.StartStreamAsync(); // Connect to the stream.
```

Send a text message:
```csharp
// Send a text message to the phone number +1 (800) 555-5555 saying this message and as an iMessage.
context.SendMessageAsync("+18005555555", "Hey dude! What's going on later?", false);
```