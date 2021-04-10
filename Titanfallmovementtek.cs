using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TitanfallDoubleJump : UdonSharpBehaviour
{
    private int JumpCount = 0;
    private float TimeWaited = 0;
    private bool IDetection = false;
    private bool trueground = true;
    public int JumpsAllowed = 2;
    public float JumpPower = 6;
    public float WaitTime = 0.5f;
    void Start()
    {
        Debug.Log("TF Movement Tech started");
        Debug.Log("Writen by Malthbern#0233");
        Debug.Log("https://github.com/Malthbern/TitanfallMovementTek");
    }
    void Update()
    {
        //this timer fixes the "Super Jump" bug to an extent
        if (IDetection == true)
        {
            if (TimeWaited < WaitTime)
            {
                Debug.Log("TimeWaited Tic");
                TimeWaited += Time.deltaTime;
            }
            else
            {
                Debug.Log("jump wait finished");
                IDetection = false;
            }
        }

        //Simple input check
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            IDetection = true;

            if (JumpCount == 0)
            {
                Debug.Log("first jump loged");
                JumpCount = 1;
            }
            else
            {
                if (TimeWaited >= WaitTime)
                {
                    Debug.Log("Djump start");
                    TimeWaited = 0;
                    IDetection = false;
                    DoJumpLogic();
                }
            }
        }
    }
    private void DoJumpLogic()
    {
        Debug.Log("start of DJMP");

        if (JumpCount < JumpsAllowed)
        {
            Debug.Log("appling DJMP");
            //sets player's V3 to their V3 + what's set in Jumppower's float value
            //also incriments the jump count to avoid infinate jumps
            Networking.LocalPlayer.SetVelocity(Networking.LocalPlayer.GetVelocity() + new Vector3(0, JumpPower, 0));
            JumpCount += 1;
        }
    }
    public void GeneralReset()
    {
        JumpCount = 0;
        TimeWaited = 0;
        Debug.Log("Jump reset by outside script");
    }
    void LateUpdate()
    {
        //this looks dumb because VRC is dumb and dosen't instantly change "isplayergrounded"
        //to false so i need to make a frame buffer to fix this
        if (trueground == false)
        {
            if (Networking.LocalPlayer.IsPlayerGrounded() == true)
            {
                Debug.Log("jump ended");
                JumpCount = 0;
                trueground = true;
                IDetection = false;
            }
        }
        else
        {
            trueground = Networking.LocalPlayer.IsPlayerGrounded();
        }
    }
}
