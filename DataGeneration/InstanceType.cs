namespace JobShopSchedulingFramework.DataGeneration
{
    /*
     InstanceType

     Defines the different categories of artificial test instances.

     It is separated from InstanceGeneratorAdvanced because:
     - the enum is a configuration concept
     - the generator should only contain generation logic
     - other classes can reuse InstanceType later
    */
    public enum InstanceType
    {
        Normal,              // Balanced processing times and moderate setup times
        LongProcessingTimes, // Longer processing times
        SetupHeavy,          // Higher setup times compared to processing times
        BottleneckMachine,   // One machine is used very often and becomes critical
        MixedRealistic       // Mix of normal and long operations
    }
}