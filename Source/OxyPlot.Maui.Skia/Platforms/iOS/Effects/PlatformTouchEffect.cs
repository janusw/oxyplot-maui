using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Platform;
using OxyPlot.Maui.Skia.Effects;
using UIKit;

namespace OxyPlot.Maui.Skia.ios.Effects;

public class PlatformTouchEffect : PlatformEffect
{
    private UIView view;
    private TouchRecognizer touchRecognizer;

    protected override void OnAttached()
    {
        view = Control ?? Container;

        var touchEffect = Element.Effects.OfType<MyTouchEffect>().FirstOrDefault();

        if (touchEffect != null && view != null)
        {
            touchRecognizer = new TouchRecognizer(Element, touchEffect);
            view.AddGestureRecognizer(touchRecognizer);
        }
    }

    protected override void OnDetached()
    {
        if (touchRecognizer != null)
        {
            touchRecognizer.Detach();
            view.RemoveGestureRecognizer(touchRecognizer);
        }
    }
}

internal class TouchRecognizer : UIGestureRecognizer
{
    private readonly Microsoft.Maui.Controls.Element element;
    private readonly MyTouchEffect touchEffect;
    private uint activeTouchesCount = 0;

    public TouchRecognizer(Microsoft.Maui.Controls.Element element, MyTouchEffect touchEffect)
    {
        this.element = element;
        this.touchEffect = touchEffect;

        ShouldRecognizeSimultaneously = new UIGesturesProbe((_, _) => true);
    }

    public void Detach()
    {
        ShouldRecognizeSimultaneously = null;
    }

    public override void TouchesBegan(NSSet touches, UIEvent evt)
    {
        base.TouchesBegan(touches, evt);
        activeTouchesCount += touches.Count.ToUInt32();
        FireEvent(touches, TouchActionType.Pressed, true);
    }

    public override void TouchesMoved(NSSet touches, UIEvent evt)
    {
        base.TouchesMoved(touches, evt);

        if (activeTouchesCount == touches.Count.ToUInt32())
        {
            FireEvent(touches, TouchActionType.Moved, true);
        }
    }

    public override void TouchesEnded(NSSet touches, UIEvent evt)
    {
        base.TouchesEnded(touches, evt);
        activeTouchesCount -= touches.Count.ToUInt32();
        FireEvent(touches, TouchActionType.Released, false);
    }

    public override void TouchesCancelled(NSSet touches, UIEvent evt)
    {
        base.TouchesCancelled(touches, evt);
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
        touchEffect.OnTouchAction(element, new(id, actionType, points, isInContact));
    }
}
