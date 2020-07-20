#include "weight-balanced.hpp"
#include "stdlib.h"

Node *fromList(Node *parent, Node **list, int count, int right)
{
    if (count == 0)
        return nullptr;
    Node *middle = list[(count + 1) / 2];
    if (!parent)
    {
        middle->depth = 0;
        middle->key = 1 << 63;
        middle->parent = nullptr;
    }
    else
    {
        middle->depth = parent->depth + 1;
        middle->key = parent->key + right * (1 << (63 - middle->depth));
        middle->parent = parent;
    }
    middle->l = fromList(middle, list, count / 2, 0);
    middle->r = fromList(middle, list + 1 + count / 2, (count - 1) / 2, 1);
    middle->size = count;
    return middle;
}

void preorder(Node &treeNode, Node **list)
{
    if (treeNode.l)
    {
        preorder(*treeNode.l, list);
        list += treeNode.l->size;
    }
    list[0] = &treeNode;
    if (treeNode.r)
        preorder(*treeNode.r, list + 1);
}

void rebuildAt(Node &treeNode)
{
    Node *parent = treeNode.parent;
    Node **nodes = (Node **)malloc(sizeof(Node *) * treeNode.size);
    preorder(treeNode, nodes);
    if (!parent)
    {
        fromList(nullptr, nodes, treeNode.size, 1);
    }
    else if (parent->l == &treeNode)
        parent->l == fromList(parent, nodes, treeNode.size, 0);
    else
        parent->r == fromList(parent, nodes, treeNode.size, 1);
    free(nodes);
}

Node *insert(Node &treeNode, tkey key)
{
    Node current = treeNode, *target = nullptr;
    while (current.parent)
        current = *current.parent;
    while (1)
    {
        if (key < current.key)
        {
            if (current.l)
            {
                current = *current.l;
                continue;
            }
            current.l = new Node();
            current.l->size = 1;
            current.l->key = current.key;
            current.l->depth = current.depth + 1;
            current.l->parent = &current;
            current.size++;
            break;
        }
        else
        {
            if (current.r)
            {
                current = *current.r;
                continue;
            }
            current.r = new Node();
            current.r->size = 1;
            current.r->key = current.key + (1 << 63 - current.depth);
            current.r->depth = current.depth + 1;
            current.r->parent = &current;
            current.size++;
            break;
        }
    }
    while (current.parent)
    {
        current.parent->size++;
        current = *current.parent;
    }
    while (&current != target)
    {
        if (key > current.key)
        {
            if (current.r->size * alpha1 > current.size * alpha2)
            {
                rebuildAt(current);
                break;
            }
            current = *current.r;
        }
        else
        {
            if (current.l->size * alpha1 > current.size * alpha2)
            {
                rebuildAt(current);
                break;
            }
            current = *current.l;
        }
    }
    return target;
}