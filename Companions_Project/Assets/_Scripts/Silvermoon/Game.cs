using System.Collections;

namespace Silvermoon.Core
{
    public interface ICompanionComponent { }

    public interface ISystem  : ICompanionComponent
    {
        void Initialize() { }
        void Cleanup() { }
    }
    
    public interface IGame
    {
        IEnumerator Initialize();
        void Quit();
    }
}