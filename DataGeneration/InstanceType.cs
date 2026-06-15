namespace JobShopSchedulingFramework.DataGeneration
{

    /*
  Defines the routing structure of a generated
  Job Shop Scheduling instance.

  Partial:
  - A job visits only a subset of the available machines.
  - The number of operations is smaller than or equal to
    the number of machines.

  Full:
  - A job visits every machine exactly once.
  - The number of operations equals the number of machines.

  The actual instance size is specified separately through
  the number of jobs and machines.
 */
    public enum TestInstanceType
    {
        Partial,
        Full
    }
}