using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Titanfall1Movement : UdonSharpBehaviour
{
    //common values
    private Vector3 CurrentV;


    //Values For DJump
    private int JumpCount = 0;
    private float JumpTimeWaited = 0;
    private bool IDetection = false;
    private bool trueground = true;

    [Tooltip("How mant jumps do you want to be able to do?")]
    public int JumpsAllowed = 2;
    [Tooltip("How high do you want to jump? (should generally be the same as your vrc value)")]
    public float JumpPower = 6;
    [HideInInspector] public float JumpWaitTime = 0.45f;

    //Values for wallrun
    private bool isWallruning = false;
    private float HungTime;
    private bool HangTimerUp = false;

    [HideInInspector] public string WallDirection;
    [Tooltip("Wall slip speed")]
    public float HangPower = 0.16f;
    [Tooltip("How long before Jumpkit gives out")]
    public float Hangtime = 4.5f;


    //these are for an admin pannel to allow for testing and addressing mov
    [HideInInspector]
    public bool ADVMVMT = true;
    [HideInInspector]
    public bool JMP = true;
    [HideInInspector]
    public bool WALL = true;


    void Start()
    {
        //credits
        Debug.Log("TF Movement Tek started");
        Debug.Log("Writen by Malthbern#0233");
        Debug.Log("https://github.com/Malthbern/TitanfallMovementTek");
    }
    void Update()
    {
        //common update stuff

        //get velocity
        CurrentV = Networking.LocalPlayer.GetVelocity();

        //a dum way to allow for scripting custom events in the way i needed
        //in U# in a way U# allows for... i hate this

        if (ADVMVMT)
        {
            if (JMP)
            {
                UpdateDJump();
            }
            if (WALL)
            {
                WallrunUpdate(); 
            }
        }
    }
    void LateUpdate()
    {
        if (ADVMVMT)
        {
            if (JMP)
            {
                LateUpdateDJump(); 
            }
        }
    }


    //beginig of all jump logic
    private void UpdateDJump()
    {
        //this timer fixes the "Super Jump" bug to an extent
        if (IDetection == true)
        {
            if (JumpTimeWaited < JumpWaitTime)
            {
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

        if (JumpCount < JumpsAllowed && isWallruning == false)
        {
            Debug.Log("appling DJMP");
            //sets player's V3 to their V3 + what's set in Jumppower's float value
            //also incriments the jump count to avoid infinate jumps
            Networking.LocalPlayer.SetVelocity(Networking.LocalPlayer.GetVelocity() + new Vector3(0, JumpPower, 0));
        }
        else if (isWallruning == true)
        {
            JumpCount = 1;
        }
    }
    private void GeneralReset()
    {
        //this is to reset for wallrunning
        JumpCount = 0;
        JumpTimeWaited = 0;
        Debug.Log("Jump reset by outside script");
    }
    private void LateUpdateDJump()
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


    // Wallrun logic
    private void WallrunUpdate()
    {
        Debug.Log("hangtimer is " + HungTime);
        //limiting how long a player can wallrun
        if (isWallruning == true && HungTime <= Hangtime)
        {
            Debug.Log("han time tick");
            HungTime += Time.deltaTime;
        }
        else if (HungTime >= Hangtime && Networking.LocalPlayer.IsPlayerGrounded() == false)
        {
            HangTimerUp = true;
            WallDirection = null;
            isWallruning = false;
            Networking.LocalPlayer.SetGravityStrength(1f);
        }

        //initate wallrun by detecting wall direction and appling a "slowed Y force"
        if (Networking.LocalPlayer.IsPlayerGrounded() == false && isWallruning == false && HangTimerUp == false)
        {
            Debug.Log("can wallrun");
            if (WallDirection == "Back")
            {
                Debug.Log("wall Direction is " + WallDirection);
                Networking.LocalPlayer.SetGravityStrength(HangPower);
                Networking.LocalPlayer.SetVelocity(new Vector3(CurrentV.x, 3f, CurrentV.z));
                isWallruning = true;
            }
            else if (WallDirection == "Right")
            {
                Debug.Log("wall Direction is " + WallDirection);
                Networking.LocalPlayer.SetGravityStrength(HangPower);
                Networking.LocalPlayer.SetVelocity(new Vector3(CurrentV.x, 3f, CurrentV.z));
                isWallruning = true;
            }
            else if (WallDirection == "Left")
            {
                Debug.Log("wall Direction is " + WallDirection);
                Networking.LocalPlayer.SetGravityStrength(HangPower);
                Networking.LocalPlayer.SetVelocity(new Vector3(CurrentV.x, 3f, CurrentV.z));
                isWallruning = true;
            }
            else if (WallDirection == "Front")
            {
                Debug.Log("wall Direction is " + WallDirection);
                Networking.LocalPlayer.SetGravityStrength(HangPower);
                Networking.LocalPlayer.SetVelocity(new Vector3(CurrentV.x, 3f, CurrentV.z));
                isWallruning = true;
            }
        }
        //reseting wallrun when you get away from the wall or touch the floor
        else if (Networking.LocalPlayer.IsPlayerGrounded() == true || WallDirection == "NW")
        {
            HangTimerUp = false;
            HungTime = 0;
            WallDirection = null;
            isWallruning = false;
            Networking.LocalPlayer.SetGravityStrength(1f);
        }
    }
    ///this branch just kinda removes the whole slidehop part due to it not really being feasible with how VRC's charecter
    ///controller is exposed to Udon, or rather the lack of exposure to Udon. hence the dubbing "Titanfall1Movement" since
    ///TF|1 dosen't have sliding. SDK3 inherently supports bhopping to an extent..... if i need to add some more code to
    ///make bhopping easier i will, but for the most part im going to clean up my code above for the forseeable future.
}
