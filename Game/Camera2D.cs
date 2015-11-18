using System;
using System.Windows;
using System.Windows.Media.Media3D;
using Game.Map;

namespace Game
{
    public class Camera2D
    {
        private const double CameraSpeed = 10;
        private const int ZoomAdd = 1;

        private double _zoomSpeed;
        private readonly PerspectiveCamera _originalCamera;
        private Room _currentRoom;
        private Vector _position;
        private double _zoom;

        public bool Focused { get; private set; }
        public Vector Position { get { return _position;  } set { Move(Zoom, value); } }
        public double Zoom { get { return _zoom; } set { Move(value, Position); } }
        public Vector3D LookDirection { get { return _originalCamera.LookDirection; } }
        public Point3D Position3D { get { return _originalCamera.Position; } }

        public Camera2D(PerspectiveCamera camera)
        {
            _originalCamera = camera;
            _originalCamera.LookDirection = new Vector3D(0, 0.25, -1);

            Focused = true;
        }
        
        public void Move(double zoom, Vector position)
        {
            _position = position;
            _zoom = zoom;
            _originalCamera.Position = (Point3D) new Vector3D(position.X, position.Y, 0) + _originalCamera.LookDirection*-zoom;
        }

        public void Move(Vector direction, double speed)
        {
            Move(Zoom, Position + new Vector(direction.X, direction.Y)*speed*CompositionTargetEx.DeltaFrame);
        }

        public void Update()
        {
            if (Focused) return;

            var dirVec = _currentRoom.GetMiddle() - Position;
            var maxDim = Math.Max(_currentRoom.RoomRect.Width, _currentRoom.RoomRect.Height) + ZoomAdd;

            var zoomDiff = maxDim - (_originalCamera.LookDirection*-Zoom).Length;

            if (dirVec.Length <= CameraSpeed*CompositionTargetEx.DeltaFrame)
            {
                Position = _currentRoom.GetMiddle();

                if (zoomDiff <= _zoomSpeed*CompositionTargetEx.DeltaFrame)
                {
                    Zoom = maxDim;

                    Focused = true;
                }
            }
            else
            {
                dirVec.Normalize();
                Move(dirVec, CameraSpeed);
            }

            if (Math.Abs(zoomDiff) > _zoomSpeed*CompositionTargetEx.DeltaFrame)
            {
                Zoom += Math.Sign(zoomDiff)*_zoomSpeed*CompositionTargetEx.DeltaFrame;
            }
            
        }

        public void Focus(Room room)
        {
            Focused = false;
            _currentRoom = room;

            var maxDim = Math.Max(_currentRoom.RoomRect.Width, _currentRoom.RoomRect.Height) + ZoomAdd;

            var deltaPos = _currentRoom.GetMiddle() - Position;
            var deltaZoom = maxDim - Zoom;

            // Make the zoom and the position meet at the same time
            _zoomSpeed = Math.Abs((deltaZoom * CameraSpeed) / deltaPos.Length);
        }
    }
}
