#include <stdio.h>
#include <string.h>
int Findpattern(char pattern[],char text[]){
    int m =strlen(pattern);
    int n=strlen(text);
    for ( int i =0;i<=n-m;i++){
        int j =0;
        while(j<m && text[i+j]==pattern[j]){
            j++;
        }
        if(j==m){
            return i;
            break;
        }
    }
   
    return -1;
}
 
int main()
{
    char text[]="leetcode";
    char pattern []="leett";
    int index=Findpattern(pattern,text);
    printf("%d",index);
   
   
    return 0;
}