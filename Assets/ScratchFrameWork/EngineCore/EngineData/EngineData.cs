namespace ScratchFramework
{
    public static class EngineData 
    {
    }
    
    /// <summary>
    /// 这部分应该改成自动生成
    /// </summary>
    public enum ScratchValueType : byte
    {
        Undefined = 0,
        Boolean = 1,
        Byte = 2,
        Integer = 3,
        Float = 4,
        Vector2 = 5,
        Vector3 = 6,
        EntityRef = 7,
        AssetRef = 8,
    }

    /// <summary>
    /// 这部分应该改成自动生成
    /// </summary>
    public enum ScratchBlockType : int
    {
        __Undefined__ = 0,
        __Event__ = 10000, //事件
        OnCollisionEnter,
        OnCollisionStay,
        OnCollisionExit,
        OnObjectCreated,
        //TODO Event

        __Action__ = 20000, //行为
        ApplyForce,
        DestroyObject,
        //TODO Action

        __Control__ = 30000, //控制
        IfElse,
        RepeatAction,
        StartCountdown,
        //TODO Control

        __Condition__ = 40000, //条件，与或非
        CompareValues,
        //TODO Condition

        __GetValue__ = 50000, //取值
        GetCharacterSpeed,
        GetVectorMagnitude,
        //TODO GetValue

        __Variable__ = 60000, //变量
        IntegerValue,
        VectorValue,
        EntityValue,
        //TODO Variable
    }
}

