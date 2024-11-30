namespace Fhi.TestFramework.NHNTTT.Dtos
{
    internal record ExpirationParameters
    {
        public bool? SetExpirationTimeAsExpired { get; set; }

        public int? ExpirationTimeInSeconds { get; set; }

        public int? ExpirationTimeInDays { get; set; }

    }

}
