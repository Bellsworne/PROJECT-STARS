using Godot;
using Newtonsoft.Json;
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
    private Camera2D _camera;

    public string ID;
    public string Username;



    public override void _Ready()
    {
        _networkHelper = GetTree().Root.GetNode("MAIN").GetNode<NetworkHelper>("NetworkHelper");
        _playerSprite = GetNode<Sprite>("PlayerSprite");
        _camera = GetNode<Camera2D>("Camera2D");

        if (ID == NetworkHelper.ID)
        {
            _isMe = true;
            _camera.Current = true;
            
        } else
        {
            _camera.Current = false;
            SetPhysicsProcess(false);
        }

        GetNode<Label>(_label).Text = Username;
        

        _networkHelper.PlayerTransformSyncRecieved += OnPlayerTransformSyncRecieved;
        
        base._Ready();
    }

    private void OnPlayerTransformSyncRecieved(string id, Transform2D transform)
    {
        if (id != ID)
        {
            NetworkHelper.World.GetNode<Player>(id.ToString()).GlobalTransform = transform;
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
            var packet = new Packet("PLAYER_TRANSFORM_SYNC", new List<object> { ID.ToString(), JsonConvert.SerializeObject(GlobalTransform) });
            _networkHelper.SendPacketToServer(packet);
        }
    }

}
