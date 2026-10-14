#include <stdio.h>
#include <stdbool.h>
bool disjoint(int a[],int l1,int b[],int l2){
    for(int i=0;i<l1;i++){
        for(int j=0;j<l2;j++){
            if(a[i]==b[j]){
                return false;
            }
            
        }
    }
            return true;
}
int main(){
    int a[]={12,13,4,5,6};
    int b[]={7,8,9,0,120};

    printf("%d", disjoint(a, sizeof(a) / sizeof(a[0]),
                          b, sizeof(b) / sizeof(b[0])));
   return 0; 
}