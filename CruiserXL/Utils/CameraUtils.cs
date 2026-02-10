using UnityEngine;

namespace CruiserXL.Utils;

// huge kudos to Zaggy for being my teacher throughout this!
public static class CameraUtils
{
    private static readonly Plane[] frustumPlanes = new Plane[6];

    public static bool IsVisibleToPlayersLocalCamera(this Renderer renderer, Camera playersCamera)
    {
        var bounds = renderer.bounds;

        GeometryUtility.CalculateFrustumPlanes(playersCamera, frustumPlanes);
        if (GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
            return true;

        return false;
    }
}