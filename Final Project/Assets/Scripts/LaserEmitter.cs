using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    [Header("Beam")]
    [SerializeField] private Transform beamOrigin;      
    [SerializeField] private float maxBeamDistance = 30f;
    [SerializeField] private int maxBounces = 5;
    [SerializeField] private LayerMask beamMask;        // what the beam can hit
    [SerializeField] private LineRenderer beamLine;

    [Header("Receiver")]
    [SerializeField] private LaserReceiver receiver;

    void Update()
    {
        FireBeam();
    }

    private void FireBeam()
    {
        Vector3 origin = beamOrigin.position;
        Vector3 direction = beamOrigin.forward;

        beamLine.positionCount = 1;
        beamLine.SetPosition(0, origin);

        bool hitReceiver = false;

        for (int i = 0; i <= maxBounces; i++)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxBeamDistance, beamMask))
            {
                beamLine.positionCount++;
                beamLine.SetPosition(beamLine.positionCount - 1, hit.point);

                if (hit.collider.CompareTag("Mirror"))
                {
                    direction = Vector3.Reflect(direction, hit.normal);
                    origin = hit.point + direction * 0.001f;  
                    continue;
                }

                if (hit.collider.CompareTag("LaserReceiver"))
                {
                    hitReceiver = true;
                }

                break;  // hit something non-mirror: beam stops
            }
            else
            {
                // nothing hit: beam extends to max
                beamLine.positionCount++;
                beamLine.SetPosition(beamLine.positionCount - 1, origin + direction * maxBeamDistance);
                break;
            }
        }

        if (receiver != null)
            receiver.SetBeamHit(hitReceiver);
    }
}