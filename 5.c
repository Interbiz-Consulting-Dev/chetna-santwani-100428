#include <stdio.h>
#include <stdbool.h>
bool disjoint(int a[],int l1,int b[],int l2){
    for(int i=0;i<l1;i++){
        for(int j=0;j<l2;j++){
            if(a[i]==b[j]){
                return true;
            }
            
        }
    }
            return false;
}
int main(){
    int a[]={12,13,4,5,6};
    int b[]={4,5,6,7,8,9,0};
     int l1=sizeof(a) /sizeof(a[0]);
    int l2=sizeof(b) /sizeof(b[0]);
    printf("%d",disjoint(a,l1,b,l2));
   return 0; 
}