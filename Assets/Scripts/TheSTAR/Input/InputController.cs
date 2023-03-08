using System;
using UnityEngine;

namespace TheSTAR.Input
{
    public class InputController : MonoBehaviour
    {
        public void Init(JoystickContainer joystickContainer, IJoystickControlled j)
        {
            joystickContainer.Init(j != null ? j.JoystickInput : null);
        }
    }

    public interface IJoystickControlled
    {
        void JoystickInput(Vector2 input);
    }
}