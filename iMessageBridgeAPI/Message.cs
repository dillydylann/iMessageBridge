using System;
using System.Collections.Generic;

namespace DylanBriedis.iMessageBridge
{
    /// <summary>
    /// Represents a text message.
    /// </summary>
    public sealed class Message : IIdObject
    {
        /// <summary>
        /// The id of the message.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Who the message is from or sent to.
        /// </summary>
        public Recipient From { get; set; }
        /// <summary>
        /// Returns true if the message is sent by you.
        /// </summary>
        public bool FromMe { get; set; }
        /// <summary>
        /// The type of service the message used.
        /// </summary>
        public ServiceType ServiceType { get; set; }
        /// <summary>
        /// When the message was sent.
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// When the message was read by you or the recipient. If the recipient has read receipts off, the value will be null.
        /// </summary>
        public DateTime? DateRead { get; set; }
        /// <summary>
        /// When the message was delivered. If the message was sent using SMS, the value will be null.
        /// </summary>
        public DateTime? DateDelivered { get; set; }
        /// <summary>
        /// Returns true if the recipient or you read the message. If the recipient has read receipts off, the value will be false.
        /// </summary>
        public bool HasRead { get; set; }
        /// <summary>
        /// Returns true if the message was delivered. If the message was sent using SMS, the value will be false.
        /// </summary>
        public bool HasDelivered { get; set; }
        /// <summary>
        /// Returns true if the message was sent successfully. If the message was sent by you, the value will be false.
        /// </summary>
        public bool HasSent { get; set; }
        /// <summary>
        /// The subject of the message. Usually shown above the actual message.
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Basically the message itself.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// The list of attachments in this message.
        /// </summary>
        public List<Attachment> Attachments { get; set; }

        /// <summary>
        /// Returns the string representation of the message.
        /// </summary>
        /// <returns>Who the message is from and the message's text.</returns>
        public override string ToString()
        {
            return (FromMe ? "Me" : From.Name) + ": " + Text;
        }
    }
}
