using UnityEngine.InputSystem;

public interface IUsable
{
    void Use(InputValue value = null);
}

public interface ISecondUsable
{
    void UseSecond();
}