namespace Yacht
{
    interface IScorePlace
    {
        string Initial { get; }
        string Name { get; }
        string Desc { get; }
        bool IsOpen { get; }
        int CurrentScore { get; }
        string CurrentScoreString { get; }
        int CalculateScore(int[] dices);
        void Submit(int[] dices);
    }
}
