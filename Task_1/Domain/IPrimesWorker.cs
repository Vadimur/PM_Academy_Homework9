namespace Task_1.Domain
{
    public interface IPrimesWorker
    {
        bool IsPrime(int number);
        int[] FindPrimesInRange(int rangeFrom, int rangeTo);
    }
}