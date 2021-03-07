#include <iostream>
#include <fstream>
#include <vector>
#include <list>
#include <queue>
#include <cmath>
#include <algorithm>

using namespace std;

class state
{
public:
	int cost, level, last_node;
	vector<bool> used;
	state* father;
	vector<vector<int>> matrix;

	state(vector<bool>& used, int cost = 0, state* father = NULL)
	{
		this->cost = cost;
		this->father = father;
		this->used = used;
		last_node = 0;
		level = 1;
	}

};

#pragma region Vec3
template <typename T>
class vec3
{
public:
	T x, y, z;

	vec3()
	{
		x = y = z = 0;
	}

	vec3(T x, T y, T z)
	{
		this->x = x;
		this->y = y;
		this->z = z;
	}
};

template <typename T>
istream& operator>> (ifstream& in, vec3<T>& v)
{
	in >> v.x >> v.y >> v.z;
	return in;
}

template <typename T>
ostream& operator<< (ostream& out,const vec3<T> v)
{
	out << "(" << v.x << ", " << v.y << ", " << v.z << ")";
	return out;
}
#pragma endregion


#pragma region Functii ajutatoare
template <typename T>
double distance(vec3<T> p1, vec3<T> p2)
{
	double a, b, c;
	a = (p1.x - p2.x) * (p1.x - p2.x);
	b = (p1.y - p2.y) * (p1.y - p2.y);
	c = (p1.z - p2.z) * (p1.z - p2.z);

	return sqrt(a + b + c);
}

void print_matrix(const vector<vector<int>> matrix)
{
	for (int i = 0; i < matrix.size(); i++)
	{
		for (int j = 0; j < matrix[i].size(); j++)
		{
			cout << matrix[i][j] << " ";
		}
		cout << endl;
	}
	cout << flush;
}
#pragma endregion


struct customCmp
{
	bool operator()(const state* a, const state* b)
	{
		return a->cost > b->cost;
	}
};

void print_sol(state* st)
{
	if (st == NULL)
		return;

	print_sol(st->father);
	std::cout << st->last_node + 1 << " ";
}

int reduction(vector<vector<int>>& matrix)
{
	int n = matrix.size(), red = 0;
	for (int i = 0; i < n; i++)
	{
		int min = -1;
		for (int j = 0; j < n; j++)
		{
			if (matrix[i][j] != -1 && (matrix[i][j] < min || min == -1))
			{
				min = matrix[i][j];
			}
		}

		//nothing to do
		if (min <= 0)
			continue;

		red += min;
		for (int j = 0; j < n; j++)
		{
			if (matrix[i][j] != -1)
				matrix[i][j] -= min;
		}
	}

	for (int i = 0; i < n; i++)
	{
		int min = -1;
		for (int j = 0; j < n; j++)
		{
			if (matrix[j][i] != -1 && (matrix[j][i] < min || min == -1))
			{
				min = matrix[j][i];
			}
		}

		//nothing to do
		if (min <= 0)
			continue;

		red += min;
		for (int j = 0; j < n; j++)
		{
			if (matrix[j][i] != -1)
				matrix[j][i] -= min;
		}
	}

	return red;
}

// varfuri - lista coordonatelor varfurilor
// vecini - pentru fiecare varf, lista varfurilor cu care este vecin
template <typename T>
void Astar(const vector<vec3<T>> varfuri, const vector<vector<int>> vecini, int start, int dest)
{
	priority_queue<state*, vector<state*>, customCmp> min_heap;
	vector<vector<int>> matrix;
	
	int n = varfuri.size(), red;
	matrix.resize(n);

	//constructie matrice de adiacente
	for (int i = 0; i < n; i++)
	{
		matrix[i].resize(n, -1);

		int nr_vecini = vecini[i].size();
		for (int j = 0; j < nr_vecini; j++)
		{
			int vecin = vecini[i][j];
			matrix[i][vecin] = distance(varfuri[i], varfuri[vecin]);
		}
	}

	vector<bool> used(n, 0);
	used[start] = 1;
	//add initial state to min_heap
	state* initial_state = new state(used);
	initial_state->matrix = matrix;
	initial_state->cost = 0;

	min_heap.push(initial_state);

	//to be used after finding solutions
	int best_sol = -1;
	state* best_state = NULL;

	//Extract states one by one
	while (!min_heap.empty())
	{
		//take best state out
		state* selected_state = min_heap.top();
		min_heap.pop();

		//nod care stim 100% ca nu ne ajuta
		if (best_sol != -1 && selected_state->cost > best_sol)
		{
			delete(selected_state);
			continue;
		}

		//Calculam toti copii posibili si ii adaugam in heap
		for (int i = 1; i < n; i++)
		{
			//sarim peste nodurile in care am fost deja
			if (selected_state->used[i])
				continue;

			state* child = new state(selected_state->used, selected_state->cost, selected_state);
			child->level = selected_state->level + 1;
			child->matrix = selected_state->matrix;
			child->last_node = i;

			//last level node, we can compute real cost and add as solution
			if (selected_state->level == n - 1)
			{
				child->cost += child->matrix[selected_state->last_node][i] + child->matrix[i][0];

				if (child->cost < best_sol || best_sol == -1)
				{
					best_sol = child->cost;
					best_state = child;
				}
			}
			else
			{
				//line i is -1
				for (int j = 0; j < n; j++)
					child->matrix[selected_state->last_node][j] = -1;

				//column j is -1
				for (int j = 0; j < n; j++)
					child->matrix[j][i] = -1;

				//dont go back
				child->matrix[i][0] = -1;

				//reduce matrix
				child->cost += reduction(child->matrix);

				//not last level node, we compute cost an insert it in min_heap
				child->cost += selected_state->matrix[selected_state->last_node][i];

				//check if child can make a better solution
				if (child->cost >= best_sol && best_sol != -1)
				{
					delete(child);
					continue;
				}

				child->used[i] = true;
				min_heap.push(child);
			}
		}

	}

	//print solution
	cout << "Cost minim:" << best_sol << endl;
	print_sol(best_state);
	cout << "1" << endl;

}

#pragma region Backtracking
class DateBacktrck
{
public:
	double cost;
	vector<int> drum;

	DateBacktrck(double cost, vector<int> drum)
	{
		this->cost = cost;
		this->drum = drum;
	}

	void printDrum()
	{
		if (cost == -1)
		{
			cout << "Nu exista drum intre aceste noduri!";
			return;
		}
		cout << "Costul drumului: " << cost << endl;
		for (int nod : drum)
		{
			cout << nod << " ";
		}
	}

	bool operator<(const DateBacktrck cmp)
	{
		return this->cost < cmp.cost;
	}

};


template <typename T>
DateBacktrck Backtracking(const vector<vec3<T>> varfuri, const vector<vector<int>> vecini, int start, int dest, vector<int> drum)
{
	// am ajuns la destinatie, costul este 0
	if (start == dest)
		return DateBacktrck(0,drum);

	DateBacktrck dateFinale = DateBacktrck(INFINITY,drum);
	bool drumPosibil = false;

	//incercam sa avansam pe toti vecinii disponibli(dar nu pe cei pe care am fost deja)
	int nrVecini = vecini[start].size();
	for (int i = 0; i < nrVecini; i++)
	{
		int vecin = vecini[start][i];
		//verificam daca am mai fost in acest nod
		if (find(drum.begin(), drum.end(), vecin) != drum.end())
		{
			//vecinul e deja in drum, il sarim
			continue;
		}

		//marcam ca am fost in nodul curent
		drum.push_back(vecin);

		//continuam cautarea
		DateBacktrck datePartiale = Backtracking(varfuri, vecini, vecin, dest, drum);

		//eliminam nodul adaugat
		drum.pop_back();

		//verificam daca s-a putut ajunge
		if (datePartiale.cost == -1)
		{
			//nu am putut ajunge asa ca sarim
			continue;
		}

		//am gasit un dru
		drumPosibil = true;

		//adaugam costul de a ajunge acolo
		datePartiale.cost += distance(varfuri[start], varfuri[vecin]);

		//pastrarea costului minim
		if (datePartiale < dateFinale)
			dateFinale = datePartiale;
	}

	if (!drumPosibil)
		dateFinale.cost = -1;

	return dateFinale;
}

#pragma endregion





int main()
{
	int n;
	vector<vec3<double>> varfuri;
	vector<vector<int>> vecini;

	//deschidere stream pentru citirea datelor
	ifstream fin("date.in");

	//citire numar de varfuri
	fin >> n;

	//alocam memorie pentru varfuri
	varfuri.resize(n);
	vecini.resize(n);

	//citire coordonate varfuri
	for (int i = 0; i < n; i++)
	{
		fin >> varfuri[i];
	}

	//citire liste adiacenta
	for (int i = 0; i < n; i++)
	{
		int point, nr_neigh;
		fin >> point;
		fin >> nr_neigh;

		vecini[point].resize(nr_neigh);

		for (int j = 0; j < nr_neigh; j++)
		{
			int x;
			fin >> x;
			vecini[i][j] = x;
		}
	}

	//Astar(varfuri, vecini, 0, 4);
	vector<int> drum;
	drum.push_back(0);
	Backtracking(varfuri, vecini, 0, 4, drum).printDrum();

	return 0;
}