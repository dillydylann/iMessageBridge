using System.Collections.Generic;

namespace DylanBriedis.iMessageBridge
{
    /// <summary>
    /// Represents a conversation with you and another person or with a group of people.
    /// </summary>
    public sealed class Conversation : IIdObject
    {
        /// <summary>
        /// The id of the conversation.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The "name" of the conversation. Usually the name is shown as the person's phone number or email, or if the conversation is a group, the name may look something like this: chat539749676434700307. We recommend you use the display name when you show the conversation's name on your app.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The list of recipients in the conversation.
        /// </summary>
        public IList<Recipient> Recipients { get; set; }
        /// <summary>
        /// The list of messages in the conversation.
        /// </summary>
        public IList<Message> Messages { get; set; }
        /// <summary>
        /// The type of service the conversation uses.
        /// </summary>
        public ServiceType ServiceType { get; set; }
        /// <summary>
        /// Returns true if the conversation has a group name.
        /// </summary>
        public bool HasGroupName { get; set; }
        /// <summary>
        /// Gets the display name of the conversation. If a group name was set, the group name is returned, if not, the recipients' names are returned.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Returns the string representation of the conversation.
        /// </summary>
        /// <returns>The display name.</returns>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
