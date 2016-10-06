using System;

namespace DylanBriedis.iMessageBridge
{
    /// <summary>
    /// Represents an attachment for a message. Usually an image or a video that is sent along with the message.
    /// </summary>
    public sealed class Attachment : IIdObject
    {
        /// <summary>
        /// The id of the attachment.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The full file name of the attachment.
        /// </summary>
        public string FullFileName { get; set; }
        /// <summary>
        /// The simplified file name of the attachment.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// When the attachment was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }
        /// <summary>
        /// The attachment's mime type.
        /// </summary>
        public string MimeType { get; set; }
        /// <summary>
        /// The total bytes of the attachment.
        /// </summary>
        public long TotalBytes { get; set; }
    }
}
