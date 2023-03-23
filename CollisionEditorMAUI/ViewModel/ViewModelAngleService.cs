using CollisionEditor.Model;

namespace CollisionEditor.ViewModel
{
    public static class ViewModelAngleService
    {
        public static (byte byteAngle, string hexAngle, double fullAngle) GetAngles(byte byteAngle)
        {
            return (byteAngle, ViewModelAssistant.GetHexAngle(byteAngle), ViewModelAssistant.GetFullAngle(byteAngle));
        }

        public static (byte byteAngle, string hexAngle, double fullAngle) GetAngles(string hexAngle)
        {
            var byteAngle = ViewModelAssistant.GetByteAngle(hexAngle);
            return (byteAngle, hexAngle, ViewModelAssistant.GetFullAngle(byteAngle));
        }
    }
}
