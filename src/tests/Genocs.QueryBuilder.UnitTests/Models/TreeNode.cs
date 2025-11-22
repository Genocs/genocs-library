namespace Genocs.QueryBuilder.UnitTests.Models;

internal class TreeNode
{
    public string? Name { get; set; }
    public List<TreeNode>? ChildNodes { get; set; }
    public int OrderId { get; set; }
    public bool Valid { get; set; }

    public bool FindDescendant()
    {
        if (ChildNodes != null && ChildNodes.Any())
        {

            foreach (TreeNode node in ChildNodes)
            {
                if (!node.Valid)
                {
                    return false;
                }
                else if (node.ChildNodes != null && node.ChildNodes.Any())
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
