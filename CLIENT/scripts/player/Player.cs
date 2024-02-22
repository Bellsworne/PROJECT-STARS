using Godot;
using System;
using System.Collections.Generic;

public class Player : RigidBody2D
{
    [Export] public float Speed = 100f;
    [Export] public float SpeedDamping = 5f;
    [Export] public float RotationDamping = 10f;

    [Export] NodePath _label;

    private Sprite _playerSprite;
    private bool _isMe = false;
    private NetworkHelper _networkHelper;

    public int ID;



    public override void _Ready()
    {
        _playerSprite = GetNode<Sprite>("PlayerSprite");

        if (ID == NetworkHelper.ID)
        {
            _isMe = true;
            
        } else
        {
            SetPhysicsProcess(false);
        }

        GetNode<Label>(_label).Text = ID.ToString();

        _networkHelper = GetTree().Root.GetNode("MAIN").GetNode<NetworkHelper>("NetworkHelper");

        _networkHelper.PlayerTransformSyncRecieved += OnPlayerTransformSyncRecieved;
        
        base._Ready();
    }

    private void OnPlayerTransformSyncRecieved(int id, Transform2D transform)
    {
        if (id != ID)
        {
            GetTree().Root.GetNode("Main").GetNode("World").GetNode<Player>(id.ToString()).GlobalTransform = transform;
        }
    }


    public override void _PhysicsProcess(float delta)
    {
        HandleMovement(delta);
        HandleRotation(delta);
        SendTransformPacket();
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

    private void SendTransformPacket()
    {
        if (LinearVelocity != Vector2.Zero) 
        {
            var packet = new Packet("PlayerTransformSync", new List<object> { ID, GlobalTransform });
            _networkHelper.SendPacketToServer(packet);
        }
    }

}
