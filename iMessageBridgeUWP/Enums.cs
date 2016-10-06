namespace DylanBriedis.iMessageBridge
{
    /// <summary>
    /// The service type.
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// Apple's instant messaging service. Supported in iOS and OS X. 
        /// </summary>
        iMessage,
        /// <summary>
        /// Short Message Service. The standard text messaging service for many regular cell phones and other smartphones.
        /// </summary>
        SMS
    }

    /// <summary>
    /// Types for a bridge object.
    /// </summary>
    public enum ObjectType
    {
        /// <summary>
        /// A recipient object.
        /// </summary>
        Recipient,
        /// <summary>
        /// A conversation object.
        /// </summary>
        Conversation,
        /// <summary>
        /// A message object.
        /// </summary>
        Message,
        /// <summary>
        /// An attachment object.
        /// </summary>
        Attachment
    }

    /// <summary>
    /// Types for stream update events.
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// An object was added to the bridge database.
        /// </summary>
        Add,
        /// <summary>
        /// An object's properties was updated.
        /// </summary>
        Update,
        /// <summary>
        /// An object was removed from the bridge database.
        /// </summary>
        Remove
    }
}
