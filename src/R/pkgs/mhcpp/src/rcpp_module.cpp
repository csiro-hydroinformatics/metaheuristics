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

class Cplx : public Complex<HyperCube<double>> {
public:
	Cplx(Rcpp::Function f) 
		//complex(scores, m, q, alpha, beta,
		//&evaluator, rng, &unif,
		//fitnessAssignment, &terminationCondition)
	{}
	void evolve() {  }
	void get_population() { ; }

private:
};


RCPP_MODULE(mh){
    using namespace Rcpp ;

    Rcpp::function("hello" , &hello  , "documentation for hello ");

    // with formal arguments specification
	//Rcpp::function("foo", &foo,
	//	List::create(_["x"] = 1, _["y"] = 1.0),
	//	"documentation for foo ");

    class_<Cplx>("Complex")
    // expose the default constructor
    .constructor<Rcpp::Function>()

	.method("evolve", &Cplx::evolve, "Launch the evolution of the complex")
	.method("get_population", &Cplx::get_population, "Gets the population of the complex")
    ;
}


