using Zenject;

namespace Programental
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<CodeTyper>().AsSingle();
        }
    }
}
