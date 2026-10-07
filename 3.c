#include <stdio.h>

int minimumcost(int a[], int n)
{
    int min = a[0];
    int cost;

    for (int i = 0; i < n; i++)
    {
        if (min > a[i])
        {
            min = a[i];
        }
    }
     cost = (n - 1) * min;

    return cost;
}

int main()
{
    int arr[] = {7, 5, 2, 9};
    int n = sizeof(arr) / sizeof(arr[0]);
    int min = minimumcost(arr, n);
    printf("Minimum cost : %d", min);

    return 0;
}