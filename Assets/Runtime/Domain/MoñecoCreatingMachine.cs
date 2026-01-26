using System.Threading.Tasks;
using Runtime.Application;

namespace Runtime
{
    public class MoñecoCreatingMachine
    {
        private readonly BagOfMoñecos _bagOfMoñecos;
        private PressWitCap _pressWithCap;
        private MoñecoMachine _machine;
        
        public MoñecoCreatingMachine(BagOfMoñecos _bagOfMoñecos, int pressesToCreate, MoñecoMachine machine)
        {
            this._bagOfMoñecos = _bagOfMoñecos;
            _pressWithCap = PressWitCap.StartWith(0,1,pressesToCreate);
            _machine = machine;
        }
        
        public void CreateMoñeco()
        {
            _bagOfMoñecos.Add();
        }

        public async Task ImpulseMoñecoCreation()
        {
            _pressWithCap.Press();
            if (!_pressWithCap.Completed) return;
            await _machine.GiveBirth();
        }
        
    }
}