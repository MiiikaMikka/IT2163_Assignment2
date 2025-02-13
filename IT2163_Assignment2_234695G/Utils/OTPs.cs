namespace IT2163_Assignment2_234695G.Utils
{
    public class OTPs
    {
        public string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // Generate a 6-digit OTP
        }
    }
}
