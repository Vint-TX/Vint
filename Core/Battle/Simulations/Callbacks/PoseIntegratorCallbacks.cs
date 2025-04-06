using System.Numerics;
using BepuPhysics;
using BepuUtilities;

namespace Vint.Core.Battle.Simulations.Callbacks;

public struct PoseIntegratorCallbacks(
    float gravity
) : IPoseIntegratorCallbacks {
    public void Initialize(Simulation simulation) { }

    public void PrepareForIntegration(float dt) { }

    public void IntegrateVelocity(
        Vector<int> bodyIndices,
        Vector3Wide position,
        QuaternionWide orientation,
        BodyInertiaWide localInertia,
        Vector<int> integrationMask,
        int workerIndex,
        Vector<float> dt,
        ref BodyVelocityWide velocity) {

    }

    public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
    public bool AllowSubstepsForUnconstrainedBodies => false;
    public bool IntegrateVelocityForKinematics => false;
}
