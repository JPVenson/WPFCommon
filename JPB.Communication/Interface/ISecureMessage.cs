namespace JPB.Communication.Interface
{
    [ExportSecProvider()]
    public interface ISecureMessage
    {
        string GeneratePublicId();
        bool CheckPublicId(string promise);
        byte[] EncryptMessage(byte[] mess);
        byte[] DecryptMessage(byte[] mess);
    }
}