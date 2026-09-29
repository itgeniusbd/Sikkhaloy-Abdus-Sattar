<?php

function password_policy_error($password)
{
  $password = (string) $password;
  if (strlen($password) < 10) {
    return 'Password must be at least 10 characters.';
  }
  if (!preg_match('/[a-z]/', $password)) {
    return 'Password must include a lowercase letter.';
  }
  if (!preg_match('/[A-Z]/', $password)) {
    return 'Password must include an uppercase letter.';
  }
  if (!preg_match('/[0-9]/', $password)) {
    return 'Password must include a number.';
  }
  if (!preg_match('/[^A-Za-z0-9]/', $password)) {
    return 'Password must include a symbol (!@#$ etc).';
  }
  return '';
}
