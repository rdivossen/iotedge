// Copyright (c) Microsoft. All rights reserved.

use std::str::FromStr;
use std::fmt;

// pub const CURRENT_API_VERSION: Versions = Version2018_06_28;

#[derive(PartialOrd, PartialEq)]
pub enum Versions {
    Version2018_06_28,
    Version2018_12_30
}

impl FromStr for Versions {
    type Err = ();

    fn from_str(s: &str) -> Result<Versions, ()> {
        match s {
            "2018-06-28" => Ok(Versions::Version2018_06_28),
            "2018-12-30" => Ok(Versions::Version2018_12_30),
            _ => Err(()),
        }
    }
}

impl fmt::Display for Versions {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        match *self 
        {
            Versions::Version2018_06_28 => write!(f, "2018-06-28"),
            Versions::Version2018_12_30 => write!(f, "2018-12-30"),
        }
    }
}
