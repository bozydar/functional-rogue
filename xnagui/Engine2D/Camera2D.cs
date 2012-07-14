using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Ruminate.Engine2D
{
    public class Camera2D
    {
        private const float MinZoom = 0.02f;
        private const float MaxZoom = 20f;
        private static GraphicsDevice _graphics;

        private float 
            _currentRotation, 
            _currentZoom, 
            _maxRotation,
            _minRotation,
            _targetRotation;
        private Vector2 
            _currentPosition,
            _maxPosition, 
            _minPosition, 
            _targetPosition,
            _translateCenter,
            _screenSize;
        private bool 
            _positionTracking, 
            _rotationTracking;
        private Body 
            _trackingBody;

        public Rectangle VisibleArea {
            get {
                var inverseViewMatrix = Matrix.Invert(View);
                var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
                var tr = Vector2.Transform(new Vector2(_screenSize.X, 0), inverseViewMatrix);
                var bl = Vector2.Transform(new Vector2(0, _screenSize.Y), inverseViewMatrix);
                var br = Vector2.Transform(_screenSize, inverseViewMatrix);
                var min = new Vector2(
                    MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))), 
                    MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
                var max = new Vector2(
                    MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))), 
                    MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
                return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
            }
        }

        public Matrix View { get; private set; }
        public Matrix SimView { get; private set; }
        public Matrix SimProjection { get; set; }        

        /// <summary>
        /// Conversion from Body coordinates to pixels
        /// </summary>
        public float MeterInPixels { get; set; }

        /// <summary>
        /// The constructor for the Camera2D class.
        /// </summary>
        /// <param name="graphics">Graphics device from current game.</param>
        /// <param name="meterInPixels">Conversion from Body coordinates to pixels.</param>
        public Camera2D(GraphicsDevice graphics, float meterInPixels) {

            MeterInPixels = meterInPixels;

            _graphics = graphics;
            SimProjection = Matrix.CreateOrthographicOffCenter(
                0f,
                (_graphics.Viewport.Width / MeterInPixels),
                (_graphics.Viewport.Height / MeterInPixels), 
                0f, 
                0f,
                1f);

            SimView = Matrix.Identity;
            View = Matrix.Identity;

            _translateCenter = new Vector2(
                (_graphics.Viewport.Width / 2f) / MeterInPixels,
                (_graphics.Viewport.Height / 2f) / MeterInPixels);

            _screenSize = new Vector2(_graphics.Viewport.Width, _graphics.Viewport.Height);

            ResetCamera();
        }

        /// <summary>
        /// The current position of the camera in pixels.
        /// </summary>
        public Vector2 Position {
            get { return (_currentPosition * MeterInPixels); }
            set{
                _targetPosition = (value / MeterInPixels);
                if (_minPosition != _maxPosition) {
                    Vector2.Clamp(
                        ref _targetPosition,
                        ref _minPosition,
                        ref _maxPosition,
                        out _targetPosition);
                }
            }
        }

        /// <summary>
        /// The furthest up, and the furthest left the camera can go.
        /// if this value equals maxPosition, then no clamping will be 
        /// applied (unless you override that function).
        /// </summary>
        public Vector2 MinPosition
        {
            get { return (_minPosition * MeterInPixels); }
            set { _minPosition = (value / MeterInPixels); }
        }

        /// <summary>
        /// the furthest down, and the furthest right the camera will go.
        /// if this value equals minPosition, then no clamping will be 
        /// applied (unless you override that function).
        /// </summary>
        public Vector2 MaxPosition
        {
            get { return (_maxPosition * MeterInPixels); }
            set { _maxPosition = (value / MeterInPixels); }
        }

        /// <summary>
        /// The current rotation of the camera in radians.
        /// </summary>
        public float Rotation {
            get { return _currentRotation; }
            set {
                _targetRotation = value % MathHelper.TwoPi;
                if (_minRotation != _maxRotation) {
                    _targetRotation = MathHelper.Clamp(
                        _targetRotation, 
                        _minRotation, 
                        _maxRotation);
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum rotation in radians.
        /// </summary>
        /// <value>The min rotation.</value>
        public float MinRotation {
            get { return _minRotation; }
            set { _minRotation = MathHelper.Clamp(value, -MathHelper.Pi, 0f); }
        }

        /// <summary>
        /// Gets or sets the maximum rotation in radians.
        /// </summary>
        /// <value>The max rotation.</value>
        public float MaxRotation {
            get { return _maxRotation; }
            set { _maxRotation = MathHelper.Clamp(value, 0f, MathHelper.Pi); }
        }

        /// <summary>
        /// The current rotation of the camera in radians.
        /// </summary>
        public float Zoom {
            get { return _currentZoom; }
            set {
                _currentZoom = value;
                _currentZoom = MathHelper.Clamp(
                    _currentZoom, 
                    MinZoom,
                    MaxZoom);
            }
        }

        /// <summary>
        /// the body that this camera is currently tracking. 
        /// Null if not tracking any.
        /// </summary>
        public Body TrackingBody {
            get { return _trackingBody; }
            set {
                _trackingBody = value;
                if (_trackingBody != null) {
                    _positionTracking = true;
                }
            }
        }

        public bool EnablePositionTracking {
            get { return _positionTracking; }
            set {
                if (value && _trackingBody != null) {
                    _positionTracking = true;
                } else {
                    _positionTracking = false;
                }
            }
        }

        public bool EnableRotationTracking
        {
            get { return _rotationTracking; }
            set {
                if (value && _trackingBody != null) {
                    _rotationTracking = true;
                } else {
                    _rotationTracking = false;
                }
            }
        }

        public bool EnableTracking {
            set {
                EnablePositionTracking = value;
                EnableRotationTracking = value;
            }
        }

        public void MoveCameraMeter(Vector2 amount) {

            _currentPosition += amount;
            if (_minPosition != _maxPosition) {
                Vector2.Clamp(
                    ref _currentPosition, 
                    ref _minPosition, 
                    ref _maxPosition, 
                    out _currentPosition);
            }
            _targetPosition = _currentPosition;
            _positionTracking = false;
            _rotationTracking = false;
        }

        public void MoveCameraPixel(Vector2 amount) {

            _currentPosition += (amount / MeterInPixels);
            if (_minPosition != _maxPosition) {
                Vector2.Clamp(
                    ref _currentPosition,
                    ref _minPosition,
                    ref _maxPosition,
                    out _currentPosition);
            }
            _targetPosition = _currentPosition;
            _positionTracking = false;
            _rotationTracking = false;
        }

        public void RotateCamera(float amount) {

            _currentRotation += amount;
            if (_minRotation != _maxRotation) {
                _currentRotation = MathHelper.Clamp(_currentRotation, _minRotation, _maxRotation);
            }
            _targetRotation = _currentRotation;
            _positionTracking = false;
            _rotationTracking = false;
        }

        /// <summary>
        /// Resets the camera to default values.
        /// </summary>
        public void ResetCamera() {

            _currentPosition = Vector2.Zero;
            _targetPosition = Vector2.Zero;
            _minPosition = Vector2.Zero;
            _maxPosition = Vector2.Zero;

            _currentRotation = 0f;
            _targetRotation = 0f;
            _minRotation = -MathHelper.Pi;
            _maxRotation = MathHelper.Pi;

            _positionTracking = false;
            _rotationTracking = false;

            _currentZoom = 1f;

            SetView();
        }

        public void Jump2Target() {

            _currentPosition = _targetPosition;
            _currentRotation = _targetRotation;

            SetView();
        }

        private void SetView() {

            var matRotation = Matrix.CreateRotationZ(_currentRotation);
            var matZoom = Matrix.CreateScale(_currentZoom);
            var translateCenter = new Vector3(_translateCenter, 0f);
            var translateBody = new Vector3(-_currentPosition, 0f);

            SimView = 
                Matrix.CreateTranslation(translateBody) *
                matRotation *
                matZoom *
                Matrix.CreateTranslation(translateCenter);

            translateCenter = (translateCenter * MeterInPixels);
            translateBody = (translateBody * MeterInPixels);

            View = 
                Matrix.CreateTranslation(translateBody) *
                matRotation *
                matZoom *
                Matrix.CreateTranslation(translateCenter);
        }

        /// <summary>
        /// Moves the camera forward one timestep.
        /// </summary>
        public void Update(GameTime gameTime) {

            if (_trackingBody != null) {
                if (_positionTracking) {
                    _targetPosition = _trackingBody.Position;
                    if (_minPosition != _maxPosition) {
                        Vector2.Clamp(
                            ref _targetPosition, 
                            ref _minPosition, 
                            ref _maxPosition, 
                            out _targetPosition);
                    }
                }
                if (_rotationTracking) {
                    _targetRotation = -_trackingBody.Rotation % MathHelper.TwoPi;
                    if (_minRotation != _maxRotation) {
                        _targetRotation = MathHelper.Clamp(
                            _targetRotation, 
                            _minRotation, 
                            _maxRotation);
                    }
                }
            }
            Vector2 delta = _targetPosition - _currentPosition;
            float distance = delta.Length();
            if (distance > 0f) {
                delta /= distance;
            }
            float inertia;
            if (distance < 10f) {
                inertia = (float) Math.Pow(distance / 10.0, 2.0);
            } else {
                inertia = 1f;
            }

            float rotDelta = _targetRotation - _currentRotation;

            float rotInertia;
            if (Math.Abs(rotDelta) < 5f) {
                rotInertia = (float) Math.Pow(rotDelta / 5.0, 2.0);
            } else {
                rotInertia = 1f;
            }
            if (Math.Abs(rotDelta) > 0f) {
                rotDelta /= Math.Abs(rotDelta);
            }

            _currentPosition += 100f * delta * inertia * (float) gameTime.ElapsedGameTime.TotalSeconds;
            _currentRotation += 80f * rotDelta * rotInertia * (float) gameTime.ElapsedGameTime.TotalSeconds;

            SetView();
        }

        public Vector2 ConvertScreenToWorld(Vector2 location) {

            var t = new Vector3(location, 0);
            t = _graphics.Viewport.Unproject(t, SimProjection, SimView, Matrix.Identity);
            return new Vector2(t.X, t.Y);
        }

        public Vector2 ConvertWorldToScreen(Vector2 location) {

            var t = new Vector3(location, 0);
            t = _graphics.Viewport.Project(t, SimProjection, SimView, Matrix.Identity);
            return new Vector2(t.X, t.Y);
        }
    }
}