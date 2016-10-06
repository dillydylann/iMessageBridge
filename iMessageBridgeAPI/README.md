# iMessage Bridge .NET API

**Current version: 1.0-a1 (This is only an alpha version so changes in new versions will usually be bug fixes or fixed mistakes in the source code)**

A .NET interface for the iMessage Bridge API.

Note that .NET 4.5 is required.

## Usage/Examples

Getting started:
```csharp
// Create the context.
BridgeContext context = new BridgeContext(/* Your bridge's IP goes here */);
if (context.AuthenticationIsRequired) // Check if we have to log in.
{
    bool successLogin = false;
    while (!successLogin) // Loop if the log in fails.
    {
        Console.Write("Username: ");
        string username = Console.ReadLine(); // Enter username.
        Console.Write("Password: ");
        string password = Console.ReadLine(); // Enter password (shown).
        context.AuthenticationCredentials = new NetworkCredential(username, password);
        successLogin = context.ValidateCredentials(); // Validate username and password.
        if (successLogin)
            Console.WriteLine("Successful login!");
        else
            Console.WriteLine("Incorrect login!");
        Console.WriteLine();
    }
}
context.Init(); // Initialize the context.
// We're good to go now!
```

List all the conversations:
```csharp
foreach (var c in context.Conversations)
{
    Console.WriteLine("-- " + c.Value.DisplayName + " --");
    foreach (var m in c.Value.Messages)
        if (m.FromMe) // Is the message from you?
            Console.WriteLine("Me: " + m.Text);
        else if (m.From != null) // I don't know why but sometimes blank messages will show with no recipient.
            Console.WriteLine(m.From.Name + ": " + m.Text);
    Console.WriteLine();
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
                Console.WriteLine("New text from " + m.From.Name + ": " + m.Text);
        }
    }
	else if (ev.Error.Message.ToLower().Contains("closed")) // Reconnect when the stream closes unexpectedly.
        context.StartStream();
    else
        throw e.Error;
};
context.StartStream(); // Connect to the stream.
```

Send a text message:
```csharp
// Send a text message to the phone number +1 (800) 555-5555 saying this message and as an iMessage.
context.SendMessage("+18005555555", "Hey dude! What's going on later?", false);
```