// -*- mode: C++; c-indent-level: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
//
// rcpp_module.cpp: Rcpp R/C++ interface class library -- Rcpp Module examples
//
// Copyright (C) 2010 - 2012  Dirk Eddelbuettel and Romain Francois
//
// This file is part of Rcpp.
//
// Rcpp is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
//
// Rcpp is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Rcpp.  If not, see <http://www.gnu.org/licenses/>.

#include <Rcpp.h>
#include <sce.hpp>

std::string hello() {
    throw std::range_error( "boom" ) ;
}

//int bar(int x) {
//    return x*2;
//}
//
//double foo(int x, double y) {
//    return x * y;
//}
//
//void bla() {
//    Rprintf("hello\\n");
//}
//
//void bla1(int x) {
//    Rprintf("hello (x = %d)\\n", x);
//}
//
//void bla2( int x, double y) {
//    Rprintf("hello (x = %d, y = %5.2f)\\n", x, y);
//}

using namespace mhcpp::optimization;
using namespace mhcpp;

class VecHyperCube : public HyperCube < double > 
{
public:
	VecHyperCube() {}	
	VecHyperCube(const VecHyperCube& src) : HyperCube<double>(src)
	{
	}
};

// std::vector<IObjectiveScores<T>>
class Population
{
public:
	Population(const Rcpp::DataFrame& df, Rcpp::NumericVector minima, Rcpp::NumericVector maxima)
	{
		auto nr = df.nrows();
		int nc = df.size();
		auto names = Rcpp::as<std::vector<std::string>>(df.names());
		for (size_t row = 0; row < nr; row++)
		{
			HyperCube < double > p;
			for (size_t col = 0; col < nc; col++)
			{
				Rcpp::NumericVector v = df[names[col]];
				p.Define(names[col], minima[col], maxima[col], v[row]);
			}
			points.push_back(p);
		}
	}

	size_t size()
	{
		return points.size();
	}

	HyperCube < double > at(size_t i)
	{
		return points[i];
	}
private:
	std::vector<HyperCube < double >> points;
};

class Scores
{
public:
	Scores(const std::vector<IObjectiveScores<HyperCube < double >>>& points)
	{
	}

	size_t size()
	{
		return points.size();
	}
	
	std::vector<IObjectiveScores<HyperCube < double >>> get_all()
	{
		return points;
	}

private:
	std::vector<IObjectiveScores<HyperCube < double >>> points;
};

class Objective
{
public:
	Objective(Rcpp::Function objFunc, Rcpp::CharacterVector objName, Rcpp::LogicalVector maximizable) : f(objFunc)
	{
		this->objName = objName[0];
		this->maximizable = maximizable[0];
	}

	Rcpp::NumericVector evaluate(Rcpp::NumericVector x)
	{
		return f(x);
	}

	Rcpp::NumericVector as_numvec(HyperCube < double > x)
	{
	}

	IObjectiveScores<HyperCube < double >> evaluate_point(HyperCube < double > x)
	{
		Rcpp::NumericVector vec = as_numvec(x);
		Rcpp::NumericVector obj = evaluate(vec);
		return IObjectiveScores<HyperCube < double >>(x, objName, maximizable);
	}

	Scores evaluate_all(Population pop)
	{
		std::vector<IObjectiveScores<HyperCube < double >>> s;
		for (size_t i = 0; i < pop.size(); i++)
		{
			auto point = pop.at(i);
			s.push_back(evaluate_point(point));
		}
		return Scores(s);
	}	
private:
	Rcpp::Function f;
	std::string objName;
	bool maximizable;
};

class Cplx : public Complex<VecHyperCube> {
public:
	Cplx(Rcpp::List sysConfigs, Rcpp::Function f, int q) //: 
		//Complex<VecHyperCube>(to_scores(f, sysConfigs), create_eval(f), q)
	{
		//auto names = sysConfigs.names();
	}
	Cplx(Rcpp::DataFrame, Rcpp::Function f, int q) 
		//complex(scores, m, q, alpha, beta,
		//&evaluator, rng, &unif,
		//fitnessAssignment, &terminationCondition)
	{}
	~Cplx() {}

	void evolve() {  }
	void get_population() { ; }

private:
};


RCPP_MODULE(mh){
    using namespace Rcpp ;

	class_<Objective>("Objective")
		.constructor<Rcpp::Function, Rcpp::CharacterVector, Rcpp::LogicalVector>()
		.method("evaluate", &Objective::evaluate, "Evaluates the score given this objective and a point")
		;
	// with formal arguments specification
	//Rcpp::function("foo", &foo,
	//	List::create(_["x"] = 1, _["y"] = 1.0),
	//	"documentation for foo ");

	class_<Population>("Population")
		.constructor<Rcpp::DataFrame, Rcpp::NumericVector, Rcpp::NumericVector>();

    class_<Cplx>("Complex")
    .constructor<Rcpp::List, int>()

	.method("evolve", &Cplx::evolve, "Launch the evolution of the complex")
	.method("get_population", &Cplx::get_population, "Gets the population of the complex")
    ;


	Rcpp::function("hello", &hello, "documentation for hello ");

}


