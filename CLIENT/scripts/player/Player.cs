using Godot;
using System;

public class Player : RigidBody2D
{
    [Export] public float Speed = 100f;
    [Export] public float SpeedDamping = 5f;
    [Export] public float RotationDamping = 10f;

    private Sprite _playerSprite;

    public override void _Ready()
    {
        _playerSprite = GetNode<Sprite>("PlayerSprite");
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
        HandleMovement(delta);
        HandleRotation(delta);
        base._Process(delta);
    }

    private void HandleMovement(float delta)
    {
        Vector2 desiredVelocity = Transform.x * Input.GetAxis("back", "forward") * Speed;

        var velocityX = Mathf.Lerp(LinearVelocity.x, desiredVelocity.x, SpeedDamping * delta);
        var velocityY = Mathf.Lerp(LinearVelocity.y, desiredVelocity.y, SpeedDamping * delta);

        LinearVelocity = new Vector2(velocityX, velocityY);
    }

    private void HandleRotation(float delta)
    {
        Rotation = Mathf.LerpAngle(Rotation, (GetGlobalMousePosition() - GlobalPosition).Angle(), RotationDamping * delta);
    }

}
