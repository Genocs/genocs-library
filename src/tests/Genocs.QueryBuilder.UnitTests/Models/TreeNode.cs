namespace Genocs.QueryBuilder.UnitTests.Models;

internal class TreeNode
{
    public string? Name { get; set; }
    public List<TreeNode>? ChildNodes { get; set; }
    public int OrderId { get; set; }
    public bool Valid { get; set; }

    public bool FindDescendant()
    {
        if (ChildNodes?.Any() == true)
        {

            foreach (TreeNode node in ChildNodes)
            {
                if (!node.Valid)
                {
                    return false;
                }
                else if (node.ChildNodes?.Any() == true)
                {
                    bool tmp = node.FindDescendant();

                    if (!tmp)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}
