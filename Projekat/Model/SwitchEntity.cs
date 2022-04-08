namespace Projekat.Model
{
    public class SwitchEntity : PowerEntity
    {
        private string _status;

        public string Status { get => _status; set => _status = value; }
    }
}
