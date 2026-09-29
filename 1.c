#include <stdio.h>
#include <string.h>

void findPattern(char text[], char pattern[])
{
    int n = strlen(text);
    int m = strlen(pattern);

    for (int i = 0; i <= n - m; i++)
    {
        int j = 0;

        while (j < m && text[i + j] == pattern[j])
        {
            j++;
        }

        if (j == m)
        {
            printf("%d ", i);
        }
    }
}

int main()
{
    char text[] = "aabaacaadaabaaba";
    char pattern[] = "aaba";

    findPattern(text, pattern);

    return 0;
}