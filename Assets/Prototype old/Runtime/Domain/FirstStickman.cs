using System;
using Runtime;

public class FirstStickman: IDisposable
{
    private PartOfBody _head;
    private PartOfBody _body;
    private PartOfBody _leftLeg;
    private PartOfBody _rightLeg;
    private PartOfBody _leftArm;
    private PartOfBody _rightArm;

    public event Action OnFullBodyReady;
    public event Action OnBodyReady;
    public event Action OnArmsReady;

    public FirstStickman()
    {
        _head = PartOfBody.Head();
        _body = PartOfBody.Body();
        _leftArm = PartOfBody.LeftArm();
        _rightArm = PartOfBody.RightArm();
        _leftLeg = PartOfBody.LeftLeg();
        _rightLeg = PartOfBody.RightLeg();

        _body.OnComplete += BodyReady;
        _leftArm.OnComplete += ArmReady;
        _rightArm.OnComplete += ArmReady;
        _leftLeg.OnComplete += FullBodyReady;
        _rightLeg.OnComplete += FullBodyReady;
        _head.OnComplete += FullBodyReady;
    }

    public void PressHead() => _head.Press();
    public void PressLeftArm() => _leftArm.Press();
    public void PressRightArm() => _rightArm.Press();
    public void PressLeftLeg() => _leftLeg.Press();
    public void PressRightLeg() => _rightLeg.Press();
    public void PressBody() => _body.Press();
    public bool BodyFullfilled => _body.Fullfilled;
    public bool LeftArmFullfilled => _leftArm.Fullfilled;
    public bool RightArmFullfilled => _rightArm.Fullfilled;
    public bool LeftLegFullfilled => _leftLeg.Fullfilled;
    public bool RightLegFullfilled => _rightLeg.Fullfilled;
    public bool HeadFullfilled => _head.Fullfilled;
    public float PercentageFullfilledBody => _body.PercentageFullfilled;
    public float PercentageLeftArmFullfilled => _leftArm.PercentageFullfilled;
    public float PercentageRightArmFullfilled => _rightArm.PercentageFullfilled;
    public float PercentageLeftLegFullfilled => _leftLeg.PercentageFullfilled;
    public float PercentageRightLegFullfilled => _rightLeg.PercentageFullfilled;
    public float PercentageHeadFullfilled => _head.PercentageFullfilled;
    

    public void BodyReady()
    {
        OnBodyReady?.Invoke();
    }

    public void ArmReady()
    {
        if (!_leftArm.Fullfilled) return;
        if (!_rightArm.Fullfilled) return;
        OnArmsReady?.Invoke();
    }

    public void FullBodyReady()
    {
        if(!_head.Fullfilled) return;
        if(!_leftLeg.Fullfilled) return;
        if(!_rightLeg.Fullfilled) return;
        OnFullBodyReady?.Invoke();
    }

    public void Dispose()
    {
        _body.OnComplete -= BodyReady;
        _leftArm.OnComplete -= ArmReady;
        _rightArm.OnComplete -= ArmReady;
        _leftLeg.OnComplete -= FullBodyReady;
        _rightLeg.OnComplete -= FullBodyReady;
        _head.OnComplete -= FullBodyReady;
    }
}
