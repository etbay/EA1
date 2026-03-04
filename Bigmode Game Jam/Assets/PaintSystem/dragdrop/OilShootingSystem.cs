using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OilShootingSystem : MonoBehaviour
{
    //


    [SerializeField] ParticleSystem inkParticle;
    [SerializeField] Transform parentController;
    [SerializeField] Transform splatGunNozzle;

    

    private bool _requestedSecondaryFire = false;

    void Start()
    {
        inkParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        _isPlaying = false;
    }
    public void updateInput(CharacterInput input)
    {
        _requestedSecondaryFire = input.SecondaryFire;
    }
    private bool _isPlaying;

    void Update()
    {
        parentController.position = splatGunNozzle.position;
        parentController.rotation = Quaternion.LookRotation(splatGunNozzle.right);

        if (_requestedSecondaryFire && !_isPlaying)
        {
            inkParticle.Play();
            _isPlaying = true;
        }
        else if (!_requestedSecondaryFire && _isPlaying)
        {
            inkParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _isPlaying = false;
        }
    }
    //void Update()
    //{

    //    if (_requestedSecondaryFire)
    //    {

    //        parentController.transform.position = splatGunNozzle.position;
    //        parentController.transform.rotation = Quaternion.LookRotation(splatGunNozzle.right);

    //        inkParticle.Play();
    //    }
    //    else
    //    {
    //        inkParticle.Stop();
    //    }

    //    //Vector3 angle = parentController.localEulerAngles;
    //    //input.blockRotationPlayer = Input.GetMouseButton(0);
    //    //bool pressing = Input.GetMouseButton(0);

    //    //if (Input.GetMouseButton(0))
    //    //{
    //    //    //VisualPolish();
    //    //    input.RotateToCamera(transform);
    //    //}

    //    //if (Input.GetMouseButtonDown(0))
    //    //    inkParticle.Play();
    //    //else if (Input.GetMouseButtonUp(0))
    //    //    inkParticle.Stop();

    //    //parentController.localEulerAngles
    //    //    = new Vector3(Mathf.LerpAngle(parentController.localEulerAngles.x, pressing ? RemapCamera(freeLookCamera.m_YAxis.Value, 0, 1, -25, 25) : 0, .3f), angle.y, angle.z);
    //}




    //void VisualPolish()
    //{
    //    if (!DOTween.IsTweening(parentController))
    //    {
    //        parentController.DOComplete();
    //        Vector3 forward = -parentController.forward;
    //        Vector3 localPos = parentController.localPosition;
    //        parentController.DOLocalMove(localPos - new Vector3(0, 0, .2f), .03f)
    //            .OnComplete(() => parentController.DOLocalMove(localPos, .1f).SetEase(Ease.OutSine));

    //        impulseSource.GenerateImpulse();
    //    }

    //    if (!DOTween.IsTweening(splatGunNozzle))
    //    {
    //        splatGunNozzle.DOComplete();
    //        splatGunNozzle.DOPunchScale(new Vector3(0, 1, 1) / 1.5f, .15f, 10, 1);
    //    }
    //}

    float RemapCamera(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
