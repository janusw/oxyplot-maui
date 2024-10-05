using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Platform;
using OxyPlot.Maui.Skia.Effects;
using UIKit;

namespace OxyPlot.Maui.Skia.macOS.Effects;

public class PlatformTouchEffect : PlatformEffect
{
    private UIView _view;
    private TouchRecognizer _touchRecognizer;
    private MyTouchEffect _touchEffect;
    protected override void OnAttached()
    {
        _view = Control ?? Container;
        _touchEffect = Element.Effects.OfType<MyTouchEffect>().FirstOrDefault();

        if (_touchEffect == null || _view == null) return;

        _touchRecognizer = new TouchRecognizer(Element, _touchEffect);
        _view.AddGestureRecognizer(_touchRecognizer);
        _view.AddGestureRecognizer(GetMouseWheelRecognizer(_view));
    }

    protected override void OnDetached()
    {
        if (_touchRecognizer != null)
        {
            _touchRecognizer.Detach();
            _view.RemoveGestureRecognizer(_touchRecognizer);
        }
    }

    // https://github.com/dotnet/maui/issues/16130
    private UIPanGestureRecognizer GetMouseWheelRecognizer(UIView v)
    {
        return new UIPanGestureRecognizer((e) =>
        {
            if (e.State == UIGestureRecognizerState.Ended)
                return;

            var isZoom = e.NumberOfTouches == 0;
            if (!isZoom) return;

            var l = e.LocationInView(v);
            var t = e.TranslationInView(v);
            var deltaX = t.X / 2;
            var deltaY = t.Y / 2;
            var delta = deltaY != 0 ? deltaY : deltaX;

            var tolerance = 5;
            if (Math.Abs(delta) < tolerance) return;

            var pointerX = l.X - t.X;
            var pointerY = l.Y - t.Y;
            var locations = new[] { new Point(pointerX, pointerY) };

            var eventArgs = new TouchActionEventArgs(0, TouchActionType.MouseWheel, locations, false)
            {
                MouseWheelDelta = (int)delta
            };

            _touchEffect.OnTouchAction(Element, eventArgs);
        })
        {
            AllowedScrollTypesMask = UIScrollTypeMask.Discrete | UIScrollTypeMask.Continuous,
            MinimumNumberOfTouches = 0,
            ShouldRecognizeSimultaneously = (_, _) => true
        };
    }
}

internal class TouchRecognizer : UIGestureRecognizer
{
    private readonly Microsoft.Maui.Controls.Element _element;
    private readonly MyTouchEffect _touchEffect;
    private uint _activeTouchesCount = 0;

    public TouchRecognizer(Microsoft.Maui.Controls.Element element, MyTouchEffect touchEffect)
    {
        this._element = element;
        this._touchEffect = touchEffect;
        ShouldRecognizeSimultaneously = (_, _) => true;
    }

    public void Detach()
    {
        ShouldRecognizeSimultaneously = null;
    }

    public override void TouchesBegan(NSSet touches, UIEvent evt)
    {
        base.TouchesBegan(touches, evt);
        _activeTouchesCount += touches.Count.ToUInt32();
        FireEvent(touches, TouchActionType.Pressed, true);
    }

    public override void TouchesMoved(NSSet touches, UIEvent evt)
    {
        base.TouchesMoved(touches, evt);

        if (_activeTouchesCount == touches.Count.ToUInt32())
        {
            FireEvent(touches, TouchActionType.Moved, true);
        }
    }

    public override void TouchesEnded(NSSet touches, UIEvent evt)
    {
        base.TouchesEnded(touches, evt);
        _activeTouchesCount -= touches.Count.ToUInt32();
        FireEvent(touches, TouchActionType.Released, false);
    }

    private void FireEvent(NSSet touches, TouchActionType actionType, bool isInContact)
    {
        UITouch[] uiTouches = touches.Cast<UITouch>().ToArray();
        long id = ((IntPtr)uiTouches.First().Handle).ToInt64();
        Point[] points = new Point[uiTouches.Length];

        for (int i = 0; i < uiTouches.Length; i++)
        {
            CGPoint cgPoint = uiTouches[i].LocationInView(View);
            points[i] = new(cgPoint.X, cgPoint.Y);
        }
        _touchEffect.OnTouchAction(_element, new(id, actionType, points, isInContact));
    }
}