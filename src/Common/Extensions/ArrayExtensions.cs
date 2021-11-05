namespace Shared.Extensions;

public static class ArrayExtensions
{
    public static bool TryInterpretAsPair(this string[] array, out string firstElement, out string secondElement)
    {
        if (array.Length != 2)
        {
            firstElement = string.Empty;
            secondElement = string.Empty;
            return false;
        }

        firstElement = array[0];
        secondElement = array[1];
        return true;
    }
}
