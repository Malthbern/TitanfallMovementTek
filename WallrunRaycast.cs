
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WallrunRaycast : UdonSharpBehaviour
{
    public UdonBehaviour BaseTFScript;
    public float RaycastDistance;

    private Ray CommonRayVars;
    private RaycastHit Right;
    private Vector3 LeftOffset = new Vector3 (0f, 180f, 0f);
    private int Bitmask = 1 << 11;


    void Update()
    {

        transform.SetPositionAndRotation(Networking.LocalPlayer.GetPosition(), Networking.LocalPlayer.GetRotation());

        //raycasts and returns direction
        //using else if to avoid multiple answers

        if (Networking.LocalPlayer.IsPlayerGrounded() == false)
        {
            RaycastHit raycast;
            if (Physics.Raycast(transform.position, Vector3.back,out raycast, RaycastDistance, Bitmask))
            {
                Debug.Log("Back cast hit");
                BaseTFScript.SetProgramVariable("WallDirection", "Back");
            }
            else if (Physics.Raycast(transform.position, transform.right, out raycast, RaycastDistance, Bitmask))
            {
                Debug.Log("Right cast hit");
                BaseTFScript.SetProgramVariable("WallDirection", "Right");
            }
            else if (Physics.Raycast(transform.position, (transform.right + LeftOffset), out raycast, RaycastDistance, Bitmask))
            {
                Debug.Log("Left cast hit");
                BaseTFScript.SetProgramVariable("WallDirection", "Left");
            }
            else if (Physics.Raycast(transform.position, Vector3.forward, out raycast, RaycastDistance, Bitmask))
            {
                Debug.Log("Front cast hit");
                BaseTFScript.SetProgramVariable("WallDirection", "Front");
            }
            else
            {
                Debug.Log("No cast hit");
                BaseTFScript.SetProgramVariable("WallDirection", "NW");
            }
        }
    }
}
