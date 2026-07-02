namespace Tools.Helpers
{
    public static class Base64Helper
    {
        public static bool IsBase64String(string text)
        {
            var bytes = new byte[text.Length * 3 / 4];
            return Convert.TryFromBase64String(text, bytes, out int bytesWritten);
        }

        public static Guid? GetGuidFromBase64(string token)
        {
            var tokenIdBytes = Convert.FromBase64String(token);
            Guid? tokenId = new Guid(tokenIdBytes);
            return tokenId;
        }

        public static string GuidToBase64(Guid id)
        {
            return Convert.ToBase64String(id.ToByteArray());
        }
    }
}
