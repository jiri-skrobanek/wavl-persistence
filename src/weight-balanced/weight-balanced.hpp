typedef unsigned long long tkey;

tkey alpha1 = 3, alpha2 = 2;

typedef struct Nodes
{
    Node *l = nullptr;
    Node *r = nullptr;
    Node *parent = nullptr;
    int size = 0;
    int depth = 0;
    tkey key = 0;
} Node;

Node *insert(Node &treeNode, tkey key);