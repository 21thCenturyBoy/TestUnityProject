namespace TestCoreLib
{

    public interface ISystem
    {
        void AwakeSystem();
        void StartSystem();
        void UpdtaeSystem(float delta);
        void LateUpdtaeSystem(float delta);
        void FixedUpdtaeSystem(float delta);
        void DestroySystem();
    }
    public interface ISubSystem : ISystem
    {
        ISystem Parent { get; internal set; }
    }



}