using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TitanfallDoubleJump : UdonSharpBehaviour
{
    //Values For DJump
    private int JumpCount = 0;
    private float JumpTimeWaited = 0;
    private bool IDetection = false;
    private bool trueground = true;
    public int JumpsAllowed = 2;
    public float JumpPower = 6;
    public float JumpWaitTime = 0.5f;

    //Values for SlideHop
    private Vector3 SavedV3;
    private float SlideTimeWaited = 0;
    private int UpHash = Animator.StringToHash("Upright");
    public float SlideVSaveTime = 0.3f;

    void Start()
    {
        //credits
        Debug.Log("TF Double Jump started");
        Debug.Log("Writen by Malthbern#0233");
        Debug.Log("https://github.com/Malthbern/TitanfallMovementTek");
    }
    void Update()
    {
        //a dumb way to allow for scripting custom events 
        //in U# in a way U# allows for... i hate this
        UpdateDJump();
        UpdateSlideHop();
    }
    void LateUpdate()
    {
        LateUpdateDJump();
    }
    //beginig of all jump logic
    void UpdateDJump()
    {
        //this timer fixes the "Super Jump" bug to an extent
        if (IDetection == true)
        {
            if (JumpTimeWaited < JumpWaitTime)
            {
                Debug.Log("JumpTimeWaited Tic");
                JumpTimeWaited += Time.deltaTime;
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
                //basically ignore the first jump
                //vrchat handels it
                Debug.Log("first jump loged");
                JumpCount = 1;
            }
            else
            {
                //check if timer is complete
                if (JumpTimeWaited >= JumpWaitTime)
                {
                    Debug.Log("Djump start");
                    JumpTimeWaited = 0;
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
    void GeneralReset()
    {
        //this is to reset for wallrunning
        JumpCount = 0;
        JumpTimeWaited = 0;
        Debug.Log("Jump reset by outside script");
    }
    void LateUpdateDJump()
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
    //begining of all slidehop logic
    void UpdateSlideHop()
    {
        if (trueground == false)
        {
            //save previous v3 and set y to 0
            SavedV3 = Networking.LocalPlayer.GetVelocity();
            SavedV3.y = 0;
        }
        else
        {
            //buffer timer to actually allow slide hoping for limited time
            if (SlideTimeWaited < SlideVSaveTime)
            {
                SlideTimeWaited += Time.deltaTime;
            }
            else
            {
                SavedV3 = new Vector3(0, 0, 0);
            }
        }
        //this code dosen't work yet, but it checks for input and crouched
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton1) && Animator.GetFloat(UpHash) < .68) //i need to get the local player's crouched position but i have no clue where to go from here (CS0120)
        {
            Networking.LocalPlayer.SetVelocity(Networking.LocalPlayer.GetVelocity() + SavedV3);
        }
    }
}
