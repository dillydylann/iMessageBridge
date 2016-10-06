namespace DylanBriedis.iMessageBridge
{
    /// <summary>
    /// Represents a recipient. Typically a person who gets messages from you.
    /// </summary>
    public sealed class Recipient : IIdObject
    {
        /// <summary>
        /// The id of the recipient.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The phone number or email of the recipient.
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// The country of where the recipient's phone is.
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// The type of service the recipient uses.
        /// </summary>
        public ServiceType ServiceType { get; set; }
        /// <summary>
        /// The name of the recipient. If a name is not found in your contacts or is not added, the address is returned.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Returns true if this recipient has a contact picture set.
        /// </summary>
        public bool HasPicture { get; set; }

        /// <summary>
        /// Returns the string representation of the recipient.
        /// </summary>
        /// <returns>The name of the recipient.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
